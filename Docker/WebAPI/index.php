<?php
// Testdaten

include("mysql.php");
############# Testdaten

// Prüfe ob die deploy_users tabelle leer ist dann leite auf testdata.php um
$checkUser = "SELECT * FROM deploy_users";
$result = mysqli_query($connection, $checkUser);
if (mysqli_num_rows($result) == 0) {
    header('Location: testdata.php');
    exit;
}else{
    header('Location: login.php');
    exit;
}

?>
# ...