<?
session_start();

require_once '../mysql.php';
require_once '../function.php';

// ausgabe nur noch in Json
header('Content-Type: application/json');


# bei action = login soll die funktion generateToken($username, $password, $connection) aufgerufen werden. die POST Parameter sollen vorher auf sicherheit geprüft werden
if (isset($_POST['password'])) {
   $username = isset($_POST['username']) ? htmlspecialchars($_POST['username']) : '';
   $password = isset($_POST['password']) ? htmlspecialchars($_POST['password']) : '';
   $token = generateToken($username, $password, $connection);
   if($token == false) {
      echo json_encode('Access Forbidden');
      exit;

   } else {
      echo json_encode($token);
   } 
}

