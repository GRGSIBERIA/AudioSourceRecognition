
#include <Siv3D.hpp> // OpenSiv3D v0.4.3
#include "Chart.hpp"

void Main()
{
	// 背景を水色にする
	Scene::SetBackground(Palette::Black);

	// 大きさ 60 のフォントを用意
	const Font font(24);

	Microphone mic(unspecified);

	Chart<WaveSampleS16> chart(font, Vec2(400, 400), U"Waveform", U"Time [s]", U"Power [relative]");
	

	mic.start();

	while (System::Update())
	{
		auto buffer = mic.getBuffer();

		const auto frame = chart.draw(buffer, { 60, 60 });
	}

	mic.stop();
}
