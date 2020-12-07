import ConnectTagInput from "./components/tag-input"
import ConnectChart from './enhancements/charts'

document.addEventListener("DOMContentLoaded", function () {
  let listener;
  listener = () => {
    ;[...document.querySelectorAll("[data-input-type='tags-input']")].forEach(
       ConnectTagInput
     );
     [...document.querySelectorAll(".ct-chart")].forEach(ConnectChart);
     $('[data-toggle="tooltip"]').tooltip()
     bsCustomFileInput.init()
  }

  document.addEventListener("turbolinks:load", listener);
  listener()
})
