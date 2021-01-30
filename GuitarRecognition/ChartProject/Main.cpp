
#include <Siv3D.hpp> // OpenSiv3D v0.4.3
#include "Chart.hpp"

void Main()
{
	// 背景を水色にする
	Scene::SetBackground(Palette::Black);

	// 大きさ 60 のフォントを用意
	const Font font(24);

	Chart chart(font, Vec2(400, 400), U"Spectrums", U"-> Hz", U"-> Power Spectrum Density");

	while (System::Update())
	{
		const auto frame = chart.draw({ 60, 60 });
		frame.drawFrame();
	}
}
