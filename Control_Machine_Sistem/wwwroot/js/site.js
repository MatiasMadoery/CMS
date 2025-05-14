function openMenu() {
    var menu = document.getElementById("menuQuery");

    if (menu) {
        menu.classList.toggle("menuMobile-Visible");
        menu.classList.toggle("menuMobile-Invisible");
    }
}
function capitalizeFirstLetter(input) {
    input.value = input.value.charAt(0).toUpperCase() + input.value.slice(1).toLowerCase();
}

$(document).ready(function () {   

    $('.select2').each(function () {
        var url = $(this).data('url');

        $(this).select2({
            placeholder: "Buscar cliente...",
            allowClear: true,
            ajax: {
                url: url,
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return {
                        term: params.term
                    };
                },
                processResults: function (data) {
                    return {
                        results: data
                    };
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.error("Error en la petición: ", textStatus, errorThrown);
                },
                cache: true
            },
            minimumInputLength: 2,
            language: {
                inputTooShort: function () {
                    return "Escribe al menos 2 letras...";
                },
                searching: function () {
                    return "Buscando...";
                },
                noResults: function () {
                    return "No se encontraron resultados";
                }
            }
        });
    });
});

