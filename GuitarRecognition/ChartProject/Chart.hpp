#include <Siv3D.hpp>

struct BoundingBox
{
	const RectF outline;
	const RectF charting;

	BoundingBox(const RectF& outline, const RectF& charting)
		: outline(outline), charting(charting) {}
};

struct ChartColoring
{
	ColorF frameColor = Palette::White;
	ColorF bgColor = ColorF(0, 0, 0, 0);
	ColorF fontColor = Palette::White;
};

struct Limit
{
	const bool hasLimit = false;
	const double maxlim = 0.0;
	const double minlim = 0.0;

	Limit() {}

	Limit(const double min, const double max)
		: hasLimit(true), maxlim(max), minlim(min) {}
};

template <class T>
struct PlotSetting
{
	/* 描画対象 */
	const T const * plotTarget;

	/* 描画する数 */
	const size_t count;

	/* 1個のデータが使う横幅 */
	const double plotDelta;

	PlotSetting(const T const * target, const size_t count, const double chartWidth)
		: plotTarget(target), count(count), plotDelta(chartWidth / count) {}
};

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

	ChartColoring color;
	Limit xlim;
	Limit ylim;

	bool isDrawOutlineFrame = true;
	bool isDrawChartingFrame = true;
	bool hasBackground = false;

public:
	void setTitle(const String& text)
	{
		initializeLabel(this->title, text, titletex, titleimg);
	}
	void setXLabel(const String& text)
	{
		initializeLabel(this->xlabel, text, xtex, ximg);
	}
	void setYLabel(const String& text)
	{
		initializeLabel(this->ylabel, text, ytex, yimg);
	}

private:
	void initializeLabel(String& basename, const String& setname, Texture& tex, Image& img)
	{
		basename = setname;
		const auto myregion = font(basename).region();
		img.resize(myregion.w, myregion.h);
		img.fill(Palette::Black);
		font(basename).overwrite(img);
		tex = Texture(img);
	}

public:
	/**
	 * チャートを定義するだけでラベルなしのチャートを描画する
	 */
	Chart(const Font& font, const Vec2& region)
		: font(font), region({ 0, 0 }, region), title(U""), xlabel(U""), ylabel(U"") {}

	/**
	 * 各種ラベルありのチャートを描画する
	 */
	Chart(const Font& font, const Vec2& region, const String& title, const String& xlabel, const String& ylabel)
		: font(font), region({ 0, 0 }, region)
	{
		setTitle(title);
		setXLabel(xlabel);
		setYLabel(ylabel);
	}

	const BoundingBox draw(const Vec2& pos = { 0, 0 }, const double padding = 8)
	{
		region.pos = pos;	// regionに描画位置をセットする

		const RectF treg = titletex.drawAt(region.topCenter(), color.fontColor);
		const RectF yreg = ytex.rotated(-90_deg).drawAt(region.leftCenter(), color.fontColor).asPolygon().boundingRect();
		const RectF xreg = xtex.drawAt(region.bottomCenter(), color.fontColor);

		region.drawFrame();

		const auto outline = RectF(
			Vec2{
				yreg.leftCenter().x,
				treg.topCenter().y
			},
			Vec2{
				region.rightCenter().x - yreg.leftCenter().x,
				xreg.bottomCenter().y - treg.topCenter().y
			}
		);

		const auto chartline = RectF(
			Vec2{
				yreg.rightCenter().x + padding,
				treg.bottomCenter().y + padding
			},
			Vec2{
				(region.rightCenter().x - padding) - (yreg.rightCenter().x + padding),
				(xreg.topCenter().y - padding) - (treg.bottomCenter().y + padding)
			}
		);

		if (isDrawOutlineFrame)
			outline.drawFrame(1, color.frameColor);

		if (isDrawChartingFrame)
			chartline.drawFrame(1, color.frameColor);

		return BoundingBox(outline, chartline);;
	}

};