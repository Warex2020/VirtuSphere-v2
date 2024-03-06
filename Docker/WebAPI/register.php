<?php
// FILEPATH: /c:/Users/dario/source/repos/VirtuSphere/VirtuSphere/Docker/Dockerfile/WebAPI/register.php
# Database:
# table: deploy_users
# id, name, password, email, created_at, updated_at
#1

function newUser($username, $password, $email, $connection) {
    $hashedPassword = password_hash($password, PASSWORD_DEFAULT);
    $query = "INSERT INTO deploy_users (name, password, email) VALUES ('$username', '$password', '$email')";
    $result = mysqli_query($connection, $query);
    if ($result) {
        return true;
    } else {
        return false;
    }
}

// Include mysql.php file
require_once 'mysql.php';

// Check if the form is submitted
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    // Get the form data
    $username = $_POST['username'];
    $password = $_POST['password'];
    $email = $_POST['email'];
    // ... add more fields as needed

    // Perform validation and insert the user into the database
    newUser($username, $password, $email, $connection);

    // Redirect to a success page
   // header('Location: success.php');
    exit;
}
?>

<!DOCTYPE html>
<html>
<head>
    <title>User Registration</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f2f2f2;
        }

        h1 {
            text-align: center;
            color: #333;
        }

        form {
            max-width: 400px;
            margin: 0 auto;
            padding: 20px;
            background-color: #fff;
            border-radius: 5px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }

        label {
            display: block;
            margin-bottom: 10px;
            color: #333;
        }

        input[type="text"],
        input[type="email"],
        input[type="password"] {
            width: 100%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-sizing: border-box;
            margin-bottom: 20px;
        }

        button[type="submit"] {
            background-color: #4CAF50;
            color: #fff;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }

        button[type="submit"]:hover {
            background-color: #45a049;
        }
    </style>
</head>
<body>
    <h1>User Registration</h1>
    <form method="POST" action="register.php">
        <label for="username">Username:</label>
        <input type="text" name="username" id="username" required><br>

        <label for="email">Email:</label>
        <input type="email" name="email" id="email" required><br>

        <label for="password">Password:</label>
        <input type="password" name="password" id="password" required><br>

        <!-- Add more form fields as needed -->

        <button type="submit">Register</button>
    </form>
</body>
</html>
