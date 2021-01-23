
# include <Siv3D.hpp> // OpenSiv3D v0.4.3
# include "TinyASIO/TinyASIO.hpp"

# include "DriverDropdown.hpp"
# include "State.hpp"

#include "GodStructure.hpp"

#include <iostream>

void Main()
{
	// 背景を水色にする
	Window::SetTitle(U"Guitar Recognition");
	Window::Resize(Size{ 1024, 768 });

	Scene::SetBackground(Palette::Black);

	GodStructure god;

	if (s3d::FileSystem::Exists(U"./settings.cfg"))
	{

	}
	else
	{
		
	}

	while (System::Update())
	{
		transition(god);
	}
}
