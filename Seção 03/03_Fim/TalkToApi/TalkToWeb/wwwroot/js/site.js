function TesteCors() {
    var tokenJWT = "";
    var servico = "https://localhost:44327/api/mensagem/17d92682-ab6e-4c43-b9fe-4618acc8ea6f/42a26526-16df-4a39-88d2-ecc36b746bea";
    $("#resultado").html("---Solicitando---");

    $.ajax({
        url: servico,
        method: "GET",
        crossDomain: true,
        headers: { "Accept": "application/json" },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Bearer " + tokenJWT);
        },
        success: function (data, status, xhr) {
            $("#resultado").html(data);
        }
    });
}
