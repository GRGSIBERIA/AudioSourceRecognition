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

	TextEditState samplingFrequencyText;
	TextEditState madohabaText;
	TextEditState shiftTimeText;

	int samplingFrequency;
	int madohaba;
	int shiftTime;

	GodStructure()
		: 
		font24(Font(24)), font18(Font(18)), font14(Font(14)), 
		samplingFrequency(48000), madohaba(16384), shiftTime(4),
		madohabaText(), shiftTimeText(), samplingFrequencyText()
	{
		samplingFrequencyText.text = U"{}"_fmt(samplingFrequency);
		madohabaText.text = U"{}"_fmt((int)Math::Log2(madohaba));
		shiftTimeText.text = U"{}"_fmt((int)Math::Log2(shiftTime));
	}

	void update()
	{
		samplingFrequency = Parse<int>(samplingFrequencyText.text);
		madohaba = 1 << Parse<int>(madohabaText.text);
		shiftTime = 1 << Parse<int>(shiftTimeText.text);
	}

	
};