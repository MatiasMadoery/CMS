function removeFile(fileUrl, rowId) {
    if (!fileUrl) return;

    if (confirm("¿Estás seguro de que querés eliminar este archivo?")) {
        console.log("Enviando solicitud de eliminación para:", fileUrl); // Log de depuración

        fetch('/Machines/DeleteFile', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ fileUrl: fileUrl })
        })
            .then(response => {
                console.log("Respuesta recibida:", response); // Log de la respuesta
                return response.json();
            })
            .then(data => {
                console.log("Datos recibidos:", data); // Log de los datos recibidos
                if (data.success) {
                    const row = document.getElementById(rowId);
                    if (row) row.remove();
                } else {
                    alert("Error al eliminar el archivo.");
                }
            })
            .catch(error => {
                console.log("Error en la solicitud:", error); // Log de error
                alert("Error al intentar eliminar el archivo.");
            });
    }
}
