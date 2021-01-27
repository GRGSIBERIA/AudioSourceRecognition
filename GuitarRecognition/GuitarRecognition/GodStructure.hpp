﻿#pragma once

#include <Siv3D.hpp>
#include "DriverDropdown.hpp"
#include "InputController.hpp"

struct GodStructure
{
private:
	template <typename T>
	void ReadAsStore(const JSONReader& reader, const String& path, TextEditState& state, T& datum, const bool isLog2 = false)
	{
		datum = reader[path].get<T>();

		if (isLog2)
		{
			state.text = U"{}"_fmt((int)Math::Log2(datum));
		}
		else
		{
			state.text = U"{}"_fmt(datum);
		}
	}

	void ReadJsonFile()
	{
		const JSONReader data(jsonpath);

		if (!data)
		{	// JSONファイルが存在しない
			// 初期値が代入されるだけなので何もしない
		}
		else
		{	// JSONファイルが存在する
			ReadAsStore(data, U"God.SamplingFrequency", samplingFrequencyText, samplingFrequency);
			ReadAsStore(data, U"God.Madohaba", madohabaText, madohaba, true);
			ReadAsStore(data, U"God.ShiftTime", shiftTimeText, shiftTime, true);
			ReadAsStore(data, U"God.TimeLength", timeLengthText, timeLength, false);

			const auto drivername = data[U"God.SelectedDriverName"].getString();
			driver.setShownIndex(drivername);

			input = new InputController(drivername);
			const auto channel = data[U"God.SelectedChannelName"].getString();
			input->setShownIndex(channel);
		}
	}

	void SaveJsonFile()
	{
		JSONWriter data;

		data.startObject();
		{
			data.key(U"God").startObject();
			{
				data.key(U"SamplingFrequency").write(samplingFrequency);
				data.key(U"Madohaba").write(madohaba);
				data.key(U"ShiftTime").write(shiftTime);
				data.key(U"TimeLength").write(timeLength);
				data.key(U"SelectedDriverName").write(driver.getSelectedName());
				data.key(U"SelectedChannelName").write(input->selectedChannelName());
			}
			data.endObject();
		}
		data.endObject();

		if (!data.save(jsonpath))
		{
			// なんか保存にコケた、理由はわからん
		}
	}

	InputController* input = nullptr;

public:
	const Font font24;
	const Font font18;
	const Font font14;

	DriverPathDropdown driver;
	

	TextEditState samplingFrequencyText;
	TextEditState madohabaText;
	TextEditState shiftTimeText;
	TextEditState timeLengthText;

	int samplingFrequency;
	int madohaba;
	int shiftTime;
	int timeLength;

	const String jsonpath = U"./config.json";


	GodStructure()
		: 
		font24(Font(24)), font18(Font(18)), font14(Font(14)), driver(),
		samplingFrequency(48000), madohaba(16384), shiftTime(4),
		madohabaText(), shiftTimeText(), samplingFrequencyText()
	{
		// コンストラクタ呼び出しは起動時の初期値にしておく
		samplingFrequencyText.text = U"{}"_fmt(samplingFrequency);
		madohabaText.text = U"{}"_fmt((int)Math::Log2(madohaba));
		shiftTimeText.text = U"{}"_fmt((int)Math::Log2(shiftTime));

		// 2回目以降の起動で保存するようにしておく
		ReadJsonFile();
	}

	// デストラクタでコントローラを破棄する
	virtual ~GodStructure()
	{
		if (input != nullptr)
			delete input;
	}

	void update()
	{
		// try-catchしないとParseに失敗してコケる
		try
		{
			samplingFrequency = Parse<int>(samplingFrequencyText.text);
		}
		catch (...)
		{
			samplingFrequency = 0;
		}
		try
		{
			madohaba = 1 << Parse<int>(madohabaText.text);
		}
		catch (...)
		{
			madohaba = 0;
		}
		try
		{
			shiftTime = 1 << Parse<int>(shiftTimeText.text);
		}
		catch (...)
		{
			shiftTime = 0;
		}
	}

	InputController& controller() { return *input; }

	const bool hasController() const { return input != nullptr; }

	void save()
	{
		SaveJsonFile();
	}

	void initController()
	{
		input = new InputController(driver.getSelectedName());
	}
};