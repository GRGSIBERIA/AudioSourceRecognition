#pragma once
# include <Siv3D.hpp>
# include "TinyASIO/TinyASIO.hpp"
# include <vector>


class DriverPathDropdown
{
	Array<String> list;

	size_t selector = 0;

public:
	DriverPathDropdown()
	: list()
	{
		const auto drivers = asio::Registry::GetAsioDriverPathes();
		for (int i = 0; i < drivers.Count(); ++i)
		{
			String driverName = Unicode::FromWString(drivers.Items(i).driverName);
			list.emplace_back(driverName);
		}
	}

	const String& getIndex() const { return list[selector]; }

	const RectF draw(const Vec2& pos)
	{
		SimpleGUI::RadioButtons(selector, list, pos);
		return SimpleGUI::RadioButtonsRegion(list, pos);
	}
};