// Convert time to a format of hours, minutes, seconds, and milliseconds

let connection = new signalR.HubConnectionBuilder()
  .withUrl("/Hubs/Donation")
  .build();

let formatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD'
});

function SetAmount(stateValue) {
  var percentage = stateValue.currentAmount / 6000.0;
  percentage = Math.min(Math.max(percentage, 0.0), 1.0);

  $('#thermometerMask').animate({ height: (100 * (1 - percentage)) + '%' }, 100);
}

connection.on('SetAmount', SetAmount);

async function Initiate() {
  await connection.start();
  await connection.invoke("RequestAmount");
}

Initiate();