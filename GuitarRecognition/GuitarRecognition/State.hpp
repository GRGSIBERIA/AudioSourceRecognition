#pragma once

#include <Siv3D.hpp>
#include "TinyASIO/TinyASIO.hpp"

#include "GodStructure.hpp"

enum class State
{
	Setup,
	Analyze
};

bool stateOfSetup(GodStructure& god)
{
	const auto titleArea = god.font24(U"ドライバー初期設定").draw(Vec2{ 8, 8 });

	const auto driversReg = god.driver.draw(Vec2{ titleArea.x, titleArea.y + titleArea.h + 8 });

	const auto rowBx = driversReg.x + driversReg.w + 8;	
	
	const auto samplingReg = god.font24(U"サンプリング周波数 [Hz]").draw(rowBx, 8);
	const auto rowCx = samplingReg.tr().x + 16;
	SimpleGUI::TextBox(god.samplingFrequencyText, Vec2{ rowCx, samplingReg.tr().y }, 100);

	const auto madoReg = god.font24(U"窓幅 [2^N点]").draw(Vec2{ rowBx, samplingReg.bl().y + 8 });
	SimpleGUI::TextBox(god.madohabaText, Vec2{ rowCx, madoReg.tr().y }, 100);

	const auto shiftReg = god.font24(U"窓シフト [2^N回]").draw(Vec2{ rowBx, madoReg.bl().y + 8 });
	SimpleGUI::TextBox(god.shiftTimeText, Vec2{ rowCx, shiftReg.tr().y }, 100);
	
	// 保存ボタン
	const auto savePos = Vec2{ driversReg.tr().x + 8, driversReg.br().y + 8 };
	const auto isSave = SimpleGUI::Button(U"保存", savePos);
	const auto saveReg = SimpleGUI::ButtonRegion(U"保存", savePos);

	if (isSave)
	{
		// 保存ボタンがクリックされたら、選択中の設定を保存する
	}
	
	// 実行ボタン
	const auto donePos = Vec2{ saveReg.br().x + 8, saveReg.y };
	const auto isDone = SimpleGUI::Button(U"実行", donePos);
	return isDone;
}


void transition(GodStructure& god)
{
	static State state = State::Setup;

	switch (state)
	{
	case State::Setup:
		if (stateOfSetup(god))
		{
			// ここでドライバの初期化を行う
		}
		break;

	case State::Analyze:
		break;

	default:
		break;
	}
}