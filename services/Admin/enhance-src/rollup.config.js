import resolve from '@rollup/plugin-node-resolve'
import { terser } from "rollup-plugin-terser"
import copy from "rollup-plugin-copy"

const common = [
  resolve(),
  copy({
    targets: [
      {
        src: "node_modules/chartist/dist/chartist.min.js",
        dest: "../wwwroot/js",
      },
      {
        src: "node_modules/chartist/dist/chartist.min.css",
        dest: "../wwwroot/css",
      },
      {
        src:
          "node_modules/chartist-plugin-tooltips/dist/chartist-plugin-tooltip.min.js",
        dest: "../wwwroot/js",
      },
      {
        src: "node_modules/bootstrap/dist/js/bootstrap.min.js",
        dest: "../wwwroot/js",
      },
      {
        src: "node_modules/jquery/jquery.min.js",
        dest: "../wwwroot/js",
      },
      {
        src: "node_modules/popper.js/dist/umd/popper.min.js",
        dest: "../wwwroot/js",
      },
      {
        src: "node_modules/bs-custom-file-input/dist/bs-custom-file-input.min.js",
        dest: "../wwwroot/js"
      }
    ],
  }),
];

const plugins =
  process.env.BUILD === "development" ? [...common] : [...common, terser()];

export default {
  input: "index.js",
  output: {
    file: "../wwwroot/js/enhance.js",
    format: "umd",
    sourcemap: process.env.BUILD === "development" ? "inline" : undefined,
    compact: process.env.BUILD === "development" ? undefined : true
  },
  plugins,
}
