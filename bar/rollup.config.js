import resolve from '@rollup/plugin-node-resolve'
import commonjs from '@rollup/plugin-commonjs'
import { terser } from "rollup-plugin-terser"
import replace from "@rollup/plugin-replace"
import json from '@rollup/plugin-json'
import sass from "rollup-plugin-sass"
import copy from "rollup-plugin-copy"
import serve from "rollup-plugin-serve"
import livereload from "rollup-plugin-livereload"
import strip from '@rollup/plugin-strip'

require("dotenv").config();

const commonPlugins = [
  resolve(),
  commonjs(),
  json(),
  replace({
    process: JSON.stringify({
      env: {
        API_URL: process.env.API_URL,
        API_KEY: process.env.API_KEY,
        QUEUE_URL: process.env.QUEUE_URL,
        QUEUE_USERNAME: process.env.QUEUE_USERNAME,
        QUEUE_PASSWORD: process.env.QUEUE_PASSWORD,
        NODE_ENV: process.env.NODE_ENV,
      },
    }),
  }),
  sass({
    output: true,
    output: "dist/styles.css",
  }),
  copy({
    targets: [
      { src: "src/index.html", dest: "dist/" },
      { src: "src/fonts/**/*", dest: "dist/fonts" },
      { src: "../web-shared/images/**/*", dest: "dist/images" },
    ],
  }),
];

const plugins =
  process.env.NODE_ENV === 'development'
    ? [
        ...commonPlugins,
        serve({
          contentBase: 'dist',
          historyApiFallback: false,
          historyApiFallback: '/index.html',
          host: '0.0.0.0',
          port: 3000,
        }),
        livereload(),
      ]
    : [...commonPlugins, strip(), terser()]

export default {
  input: "src/app.js",
  output: {
    file: "dist/app.bundled.js",
    format: "umd",
    sourcemap: process.env.NODE_ENV === "development" ? "inline" : undefined,
    compact: process.env.NODE_ENV === "development" ? undefined : true,
  },
  plugins,
};
