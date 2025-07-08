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

function setupWarrantyDateAutoCalc(deliveryInputId, warrantyPreviewId) {
    const deliveryInput = document.getElementById(deliveryInputId);
    const warrantyPreview = document.getElementById(warrantyPreviewId);

    if (!deliveryInput || !warrantyPreview) return;

    deliveryInput.addEventListener("change", function () {
        const deliveryDate = new Date(this.value);
        if (!isNaN(deliveryDate)) {
            const warrantyDate = new Date(deliveryDate);
            warrantyDate.setDate(warrantyDate.getDate() + 365);
            warrantyPreview.value = warrantyDate.toISOString().split("T")[0]; // ✅ solo fecha
        } else {
            warrantyPreview.value = "";
        }
    });
}
