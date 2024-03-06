<?php
session_start();

// Include mysql.php file
require_once 'mysql.php';

// Check if the user is already logged in
if (isset($_SESSION['user'])) {
    // Redirect to intern.php
    header('Location: intern.php');
    exit;
}



function loginUser($username, $password, $connection) {
    $query = "SELECT * FROM deploy_users WHERE name='$username'";
    $result = mysqli_query($connection, $query);
    if ($result && mysqli_num_rows($result) > 0) {
        $user = mysqli_fetch_assoc($result);
        if (password_verify($password, $user['password'])) {
            return true;
        }
    }
    return false;
}

// Check if the form is submitted
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    // Get the form data
    $username = $_POST['username'];
    $password = $_POST['password'];

    // Perform validation and check if the user exists in the database
    if (loginUser($username, $password, $connection)) {
        // Redirect to a success page
        $_SESSION['user'] = $username;
        header('Location: intern.php');
        exit;
    } else {
        $error = "Invalid username or password";
    }
}
?>
<!DOCTYPE html>
<html>

<head>
    <link rel="stylesheet" href="style.css">
    <title>ATEP-LOGIN</title>
    
</head>
<body>
    <h1>Willkommen! </h1> <br>
    <h2>Bitte loggen Sie sich ein</h2>
    
    <div class="container">
        <form action="landingpage.php" method="POST">
            <div class="form-group">
                <label for="username">Benutzername:</label>
                <input type="text" id="username" name="username" required>
            </div>
            <div class="form-group">
                <label for="password">Passwort:</label>
                <input type="password" id="password" name="password" required>
            </div>
            <div class="form-group">
                <button type="submit">Anmelden</button>
            </div>
        </form>
    </div>

<?php
// Check if the login form is submitted
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    // Perform login validation here
    // If login is successful, redirect to landingpage.php
    header('Location: landingpage.php');
    exit;
}
?>
</body>

</html>