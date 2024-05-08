<?php
include 'connection.php';


	$email = $_GET['email'];
	$password = $_GET['pass'];

	$query = "
	SELECT * 
	FROM user 
	WHERE Email = '". $email ."'
			";

    $result = mysqli_query($con, $query);
    $count = mysqli_num_rows($result);  
	
    if($count > 0)
    {
		$row = mysqli_fetch_assoc($result);
		$fetch_pass = $row['Password'];
        if(password_verify($password, $fetch_pass)){
			echo '/' . $row['ID'] . "|" . $row['First'] . "|" . $row['Last']. "|" . $row['Rank'];   
		}
		else{
			echo "Incorrect password.";
		}
    }
	else {
		echo "User doesnt exist.";
	}

?>
