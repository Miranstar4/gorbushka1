// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var toastElList = [].slice.call(document.querySelectorAll('.toast-save'))
var toastList = toastElList.map(function (toastEl) {
  return new bootstrap.Toast(toastEl)
})

var toastDeleteElList = [].slice.call(document.querySelectorAll('.toast-delete'))
var toastDeleteList = toastDeleteElList.map(function (toastEl) {
  return new bootstrap.Toast(toastEl)
})

function showSaveToast() {
  toastList[0].show();
}

function showDeleteToast() {
  toastDeleteList[0].show();
}