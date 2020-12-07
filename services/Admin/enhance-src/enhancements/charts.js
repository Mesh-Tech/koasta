export default function ($el) {
  if ($el.classList.contains("chart-bar")) {
    const config = JSON.parse($el.dataset.chartConfig)
    new Chartist.Bar($el, config, {
      fullWidth: true,
      plugins: [
        Chartist.plugins.tooltip({
          anchorToPoint: true,
        }),
      ],
    });
  }
}
