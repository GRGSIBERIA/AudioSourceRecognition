#pragma once

#include <Siv3D.hpp>
#include "DriverDropdown.hpp"


struct GodStructure
{
public:
	const Font font24;
	const Font font18;
	const Font font14;

	DriverPathDropdown driver;

	GodStructure()
		: font24(Font(24)), font18(Font(18)), font14(Font(14))
	{

	}
};