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
	const Font& font;
	const double maxlim = 0.0;
	const double minlim = 0.0;

	Limit() : font(Font(16)) {}

	Limit(const Font& font, const double min, const double max)
		: hasLimit(true), maxlim(max), minlim(min), font(font) {}
};

template <class T>
struct PlotSetting
{
	/* 描画対象 */
	const T const * plotTarget = nullptr;

	/* 描画する数 */
	const size_t count = 0;

	/* 1個のデータが使う横幅 */
	const double plotDelta;

	PlotSetting(const T const * target, const size_t count, const double chartWidth)
		: plotTarget(target), count(count), plotDelta(chartWidth / count) {}

	PlotSetting()
		: plotDelta(0) {}

	/**
	 * 最大値を返す
	 * @returns 最大値
	 */
	const T max() const
	{
		size_t maxid = 0;
		for (size_t i = 1; i < count; ++i)
			if (plotTarget[maxid] < plotTarget[i])
				maxid = i;
		return plotTarget[maxid];
	}

	/**
	 * 最小値を返す
	 * @returns 最小値
	 */
	const T min() const
	{
		size_t minid = 0;
		for (size_t i = 1; i < count; ++i)
			if (plotTarget[minid] > plotTarget[i])
				minid = i;
		return plotTarget[minid];
	}
};

template <class T>
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
	PlotSetting<T> plotProperty;

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

	virtual ~Chart()
	{
		
	}

public:
	void plot(const Array<T> data)
	{
		plotProperty = PlotSetting<T>(&data[0], data.size(), region.w);
	}

private:
	void drawPlotLine(const RectF& chartline)
	{
		if (plotProperty.plotTarget == nullptr) return;

		const T min = plotProperty->min();
		const T max = plotProperty->max();

		for (size_t i = 0; i < plotProperty.count; ++i)
		{

		}
	}


public:
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

		if (hasBackground)
			chartline.draw(color.bgColor);

		return BoundingBox(outline, chartline);;
	}

};