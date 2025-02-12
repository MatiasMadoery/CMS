document.addEventListener('DOMContentLoaded', function () {
    var toggler = document.querySelector('.navbar-toggler');
    var navbarNav = document.getElementById('navbarNav');

    toggler.addEventListener('click', function () {
        if (navbarNav.style.display === "block") {
            navbarNav.style.display = "none";
        } else {
            navbarNav.style.display = "block";
        }
    });
});

