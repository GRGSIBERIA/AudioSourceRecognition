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

	return god.driver.draw(Vec2{ titleArea.x, titleArea.y + titleArea.h + 8 });
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