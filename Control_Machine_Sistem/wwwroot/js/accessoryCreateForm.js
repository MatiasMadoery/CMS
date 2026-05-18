document.addEventListener("DOMContentLoaded", function () {
    const categorySelect = document.getElementById("CategoryId");
    const modelSelect = document.getElementById("AccessoryModelId");

    if (categorySelect && modelSelect) {
        categorySelect.addEventListener("change", function () {
            const categoryId = this.value;

            // Restablecer el select de modelos asociados a un estado neutro
            modelSelect.innerHTML = '<option value="">-- Seleccionar del Catálogo --</option>';

            if (!categoryId) {
                return; // Si no seleccionó nada válido, termina la ejecución
            }

            // Realizar Fetch al endpoint dinámico del controlador de accesorios
            fetch(`/Accessories/GetAccessoriesModelsByCategory?categoryId=${categoryId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error("Error en la respuesta del servidor.");
                    }
                    return response.json();
                })
                .then(data => {
                    if (data && data.length > 0) {
                        data.forEach(model => {
                            const option = document.createElement("option");
                            option.value = model.id;
                            option.text = model.name;
                            modelSelect.appendChild(option);
                        });
                    } else {
                        modelSelect.innerHTML = '<option value="">No hay modelos cargados para esta categoría</option>';
                    }
                })
                .catch(error => {
                    console.error("Error al procesar el dropdown en cascada:", error);
                });
        });
    }
});