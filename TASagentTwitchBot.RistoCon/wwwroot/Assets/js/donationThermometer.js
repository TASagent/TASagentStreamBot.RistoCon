// Convert time to a format of hours, minutes, seconds, and milliseconds

let connection = new signalR.HubConnectionBuilder()
  .withUrl("/Hubs/Donation")
  .build();

let formatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD'
});

let goalAmount = 2000;
let currentAmount = 0;

//Immediately set the state
function SetState(data) {
  currentAmount = data.newAmount;
  goalAmount = data.newGoal;
  SetBar();
}

//Sets the current funding amount
function UpdateAmount(data) {
  currentAmount = data.newAmount;
  AnimateBar();
}

function AnimateBar() {
  var percentage = currentAmount / goalAmount;
  percentage = Math.min(Math.max(percentage, 0.0), 1.0);

  $('#thermometerMask').animate({ height: (100 * (1 - percentage)) + '%' }, 500);
  $('#thermometerGrowthMask').delay(1000).animate({ height: (100 * (1 - percentage)) + '%' }, 500);
}

function SetBar() {
  var percentage = currentAmount / goalAmount;
  percentage = Math.min(Math.max(percentage, 0.0), 1.0);

  $('#thermometerMask').height(`${(100 * (1 - percentage))}%`);
  $('#thermometerGrowthMask').height(`${(100 * (1 - percentage))}%`);
}

connection.on('SetState', SetState);
connection.on('UpdateAmount', UpdateAmount);

async function Initiate() {
  await connection.start();
  await connection.invoke('RequestState');
}

Initiate();