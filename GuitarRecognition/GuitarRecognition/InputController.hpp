#pragma once

#include <Siv3D.hpp>
#include "TinyASIO/TinyASIO.hpp"
#include <memory>

class InputController : public asio::ControllerBase
{
	static asio::InputBuffer* input;

	/**
	* Stream内部に蓄積されるので適当なフレームでFetchする
	*/
	static void BufferSwitch(long index, long)
	{
		void* buffer = GetInputMemory(0, index);

		Input(0).Store(buffer, bufferLength);

		// ダブルバッファから呼び出して、中身をストリームに蓄積する
	}

	Array<String> list;
	size_t selector = 0;
	int timeLength;
	int reservedBufferSize;
	std::vector<asio::SampleType> superBuffer;

public:
	InputController(const String& driverName)
		: ControllerBase(driverName.narrow()), list(), superBuffer()
	{
		for (int i = 0; i < channelManager->NumberOfInputs(); ++i)
		{
			const String channelName = Unicode::Widen(channelManager->Inputs(i).name);
			list.emplace_back(channelName);
		}
	}

	virtual ~InputController()
	{
		try
		{
			this->DisposeBuffer();
		}
		catch (...)
		{
			// 何もせずに破棄されることを祈る
		}
	}

	const RectF draw(const Vec2& pos)
	{
		SimpleGUI::RadioButtons(selector, list, pos);
		return SimpleGUI::RadioButtonsRegion(list, pos);
	}

	/**
	* バッファを初期化した後に
	*/
	const bool initializeAsPlay(int timeLength)
	{
		const asio::InputChannel* channel = nullptr;
		for (int i = 0; i < channelManager->NumberOfInputs(); ++i)
		{
			if (list[selector] == Unicode::Widen(channelManager->Inputs(i).name))
			{
				channel = &channelManager->Inputs(i);
				CreateBuffer({ *channel }, BufferSwitch);
				break;
			}
		}

		if (channel == nullptr)
			return false;

		// カプセル化したバッファを初期化する
		this->timeLength = timeLength;
		reservedBufferSize = sizeof(asio::SampleType) * sampleRate * timeLength;
		superBuffer.reserve(sampleRate * timeLength);
		superBuffer.resize(sampleRate * timeLength, 0);

		this->Start();
		return true;
	}

	const String& selectedChannelName() const
	{
		return list[selector];
	}

	/**
	* チャンネル名を指定してインデックスを設定する
	* @param name チャンネル名
	* @returns 成功の可否
	*/
	bool setShownIndex(const String& name)
	{
		for (size_t i = 0; i < channelManager->NumberOfInputs(); ++i)
		{
			const String selectedName = Unicode::Widen(channelManager->Inputs(i).name);
			if (selectedName == name)
			{
				selector = i;
				return true;
			}
		}
		return false;
	}

	void update()
	{
		const auto ptr = input->Fetch();
		const auto val = ptr.get();
		const auto len = ptr.get()->size();

		superBuffer.assign(superBuffer.begin() + len, superBuffer.end());
		superBuffer.insert(superBuffer.end(), val->begin(), val->end());
		// バッファを前に詰めて、後ろにストリームの内容を詰める
	}
};

asio::InputBuffer* InputController::input = nullptr;
