 document.getElementById("CategoryId").addEventListener("change", function () {
      var categoryId = this.value;
      var modelSelect = document.getElementById("ModelId");
      modelSelect.innerHTML = "<option value=''>Cargando modelos...</option>";

      fetch(`/Machines/GetModelsByCategory?categoryId=${categoryId}`)
          .then(response => response.json())
          .then(data => {
              modelSelect.innerHTML = "<option value=''>Seleccione un modelo...</option>";
              data.forEach(model => {
                  modelSelect.innerHTML += `<option value="${model.id}">${model.name}</option>`;
              });
          })
          .catch(error => console.error("Error cargando modelos:", error));
 });

