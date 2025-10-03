function enviarValor() {
    var select = document.getElementById("option");
    var valorSeleccionado = select.value;
    var urlActual = window.location.href;

    fetch('/Idioma/SetLanguage', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            valor: valorSeleccionado,
            url: urlActual
        })
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('Error al enviar los datos al servidor.');
            }
        })
        .then(data => {
            console.log('Respuesta del servidor:', data);
        })
        .catch(error => {
            console.error('Error:', error);
        });
}