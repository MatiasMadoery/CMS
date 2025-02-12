function openMenu() {
    var menu = document.getElementById("menuQuery");

    if (menu) {
        menu.classList.toggle("menuMobile-Visible");
        menu.classList.toggle("menuMobile-Invisible");
    }
}