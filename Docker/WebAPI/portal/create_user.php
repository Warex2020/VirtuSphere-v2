<?php
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    // Get the user data from the request
    $name = $_POST['name'];
    $password = $_POST['password'];
    $email = $_POST['email'];

    // Connect to the database
    require_once 'mysql.php';

    // Insert the user data into the database
    $query = "INSERT INTO deploy_users (`name`, password, email) VALUES ('$name', '$password', '$email')";
    $result = mysqli_query($connection, $query);

    if ($result) {
        echo "User data inserted successfully!";
    } else {
        echo "Error inserting user data: " . mysqli_error($connection);
    }

    // Close the database connection
    mysqli_close($connection);
}
?>