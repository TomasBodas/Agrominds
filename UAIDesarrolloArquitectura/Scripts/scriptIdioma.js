function enviarValor(actionUrl) {
 var select = document.getElementById("option");
 var valorSeleccionado = select ? select.value : "";
 var urlActual = window.location.href;

 // Construir action de destino. Prioriza parametro, luego data-atributo, luego fallback
 var target = actionUrl || (select && select.getAttribute('data-language-url')) || '/Idioma/SetLanguage';

 // Realiza un POST clasico para forzar la navegacion/refresh completa
 var form = document.createElement('form');
 form.method = 'POST';
 form.action = target;

 var inputValor = document.createElement('input');
 inputValor.type = 'hidden';
 inputValor.name = 'Valor'; // coincide con propiedades del DTO
 inputValor.value = valorSeleccionado;

 var inputUrl = document.createElement('input');
 inputUrl.type = 'hidden';
 inputUrl.name = 'Url'; // coincide con propiedades del DTO
 inputUrl.value = urlActual;

 form.appendChild(inputValor);
 form.appendChild(inputUrl);

 document.body.appendChild(form);
 form.submit();
}