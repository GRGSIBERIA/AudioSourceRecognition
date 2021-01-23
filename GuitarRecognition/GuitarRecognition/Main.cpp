
# include <Siv3D.hpp> // OpenSiv3D v0.4.3
# include "TinyASIO/TinyASIO.hpp"

# include "DriverDropdown.hpp"
# include "State.hpp"

#include "GodStructure.hpp"

void Main()
{
	// 背景を水色にする
	Scene::SetBackground(Palette::Black);

	GodStructure god;

	while (System::Update())
	{
		transition(god);
	}
}
