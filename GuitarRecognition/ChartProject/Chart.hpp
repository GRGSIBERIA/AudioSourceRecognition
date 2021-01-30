#include <Siv3D.hpp>



class Chart
{
	const Font& font;
	RectF region;

	String title;
	String xlabel;
	String ylabel;

	Texture titletex;
	Texture xtex;
	Texture ytex;

	Image titleimg;
	Image ximg;
	Image yimg;

private:
	void initializeLabel(String& basename, const String& setname, Texture& tex, Image& img)
	{
		basename = setname;
		const auto region = font(basename).region();
		img.resize(region.w, region.h);
		font(basename).overwrite(img);
		tex = Texture(img);
	}

public:
	Chart(const Font& font, const RectF& region)
		: font(font), region(region) {}

	Chart(const Font& font, const RectF& region, const String& title, const String& xlabel, const String& ylabel)
		: font(font), region(region) 
	{
		setTitle(title);
		setXLabel(xlabel);
		setYLabel(ylabel);
	}

	void setTitle(const String& title) 
	{ 
		initializeLabel(this->title, title, titletex, titleimg);
	}
	void setXLabel(const String& xlabel) 
	{ 
		initializeLabel(this->xlabel, xlabel, xtex, ximg);
	}
	void setYLabel(const String& ylabel) 
	{ 
		initializeLabel(this->ylabel, ylabel, ytex, yimg);
	}

	Chart& draw(const Vec2& pos)
	{
		
	}

};