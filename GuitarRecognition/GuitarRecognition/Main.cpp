
# include <Siv3D.hpp> // OpenSiv3D v0.4.3
# include "TinyASIO/TinyASIO.hpp"

# include "DriverDropdown.hpp"

void Main()
{
	// 背景を水色にする
	Scene::SetBackground(Palette::Black);

	// 大きさ 60 のフォントを用意
	const Font font(24);

	DriverPathDropdown driver(Vec2{ 100, 100 });

	while (System::Update())
	{
		
	}
}
