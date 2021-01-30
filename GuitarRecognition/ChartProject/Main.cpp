
#include <Siv3D.hpp> // OpenSiv3D v0.4.3
#include "Chart.hpp"

void Main()
{
	// 背景を水色にする
	Scene::SetBackground(ColorF(0.8, 0.9, 1.0));

	// 大きさ 60 のフォントを用意
	const Font font(24);

	Chart chart(font);

	while (System::Update())
	{
		
	}
}
