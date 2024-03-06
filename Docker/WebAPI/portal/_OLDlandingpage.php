<?php

    // Include the mysql.php file to establish a database connection
    include 'mysql.php';

    // Query the database to get the current users
    $query = "SELECT name, email FROM users";
    $result = mysqli_query($connection, $query);

    // Check if the query was successful
    if ($result) {
        // Loop through the result set and display the users in the table
        while ($row = mysqli_fetch_assoc($result)) {
            echo "<tr>";
            echo "<td>{$row['name']}</td>";
            echo "<td>{$row['email']}</td>";
            echo "</tr>";
        }
    } else {
        // Display an error message if the query failed
        echo "Error: " . mysqli_error($connection);
    }

    // Close the database connection
    mysqli_close($connection);
    ?>
               { echo "<td>{$user['email']}</td>";
                echo "</tr>";
            }
            ?>
        </tbody>
    </table>
<!DOCTYPE html>
<html>
<head>
    <title>Landingpage</title>
    <style>
        /* CSS-Styling für das Dashboard */
        /* Fügen Sie hier Ihre eigenen Stile hinzu */
    </style>
</head>
<body>
    <h1>Wilkommen auf der Testseite</h1>
    <button>Drück mich</button>
    
    <div id="dashboard">
        <!-- Hier werden Ihre Dashboard-Elemente angezeigt -->
    </div>
    
    <script>
        // JavaScript-Code für die Interaktivität des Dashboards
        // Fügen Sie hier Ihren eigenen Code hinzu
    </script>

</body>


</html>
<button onclick="window.location.href = 'intern.php';">Zur internen Seite</button>