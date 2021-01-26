﻿#pragma once

#include <Siv3D.hpp>
#include "TinyASIO/TinyASIO.hpp"

#include "GodStructure.hpp"

enum class State
{
	Setup,
	Analyze
};

RectF HorizontalStatus(const Font& font, const String& title, const String& answer, TextEditState& state, int& value, const double rowBx, const double rowCx, const RectF& chain)
{
	const auto reg = font(title).draw(Vec2{ rowBx, chain.bl().y + 8 });
	SimpleGUI::TextBox(state, Vec2{ rowCx, reg.tr().y }, 100);
	const auto ui = SimpleGUI::TextBoxRegion(Vec2{ rowCx, reg.tr().y }, 100);
	font(answer).draw(ui.tr().x + 8, ui.tr().y);
	return reg;
}

RectF HorizontalStatus(const Font& font, const String& title, TextEditState& state, int& value, const double rowBx)
{
	const auto reg = font(title).draw(rowBx, 8);
	const auto rowCx = reg.tr().x + 16;
	SimpleGUI::TextBox(state, Vec2{ rowCx, reg.tr().y }, 100);
	return reg;
}

bool stateOfSetup(GodStructure& god)
{
	const auto titleArea = god.font24(U"ドライバー初期設定").draw(Vec2{ 8, 8 });

	const auto driversReg = god.driver.draw(Vec2{ titleArea.x, titleArea.y + titleArea.h + 8 });

	/* 繰り返しで表現している部分は関数でまとめた */

	const auto rowBx = driversReg.tr().x + 16;	
	const auto samplingReg = HorizontalStatus(god.font24, U"サンプリング周波数 [Hz]", god.samplingFrequencyText, god.samplingFrequency, rowBx);
	
	const auto rowCx = samplingReg.tr().x + 16;
	const auto madoReg = HorizontalStatus(god.font24, U"窓幅 [2^N点]", U"= {} 点"_fmt(god.madohaba), god.madohabaText, god.madohaba, rowBx, rowCx, samplingReg);
	const auto shiftReg = HorizontalStatus(god.font24, U"窓シフト [2^N回]", U"= {} 回"_fmt(god.shiftTime), god.shiftTimeText, god.shiftTime, rowBx, rowCx, madoReg);

	// 保存ボタン
	const auto savePos = Vec2{ driversReg.tr().x + 8, driversReg.br().y + 8 };
	const auto isSave = SimpleGUI::Button(U"保存", savePos);
	const auto saveReg = SimpleGUI::ButtonRegion(U"保存", savePos);

	god.update();

	if (isSave)
	{
		// 保存ボタンがクリックされたら、選択中の設定を保存する
		god.save();
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
			state = State::Analyze;
		}
		break;

	case State::Analyze:
		break;

	default:
		break;
	}
}