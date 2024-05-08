<?php
include 'connection.php';
       
	$first_name = $_GET['fn'];
    $last_name = $_GET['ln'];
    $email = $_GET['email'];
    $password = $_GET['pass'];
    $is_teacher = $_GET['teach'];

	$query = 
    "SELECT * 
     FROM user
     WHERE Email = '$email'
    ";

    $result = mysqli_query($con, $query);
    $num_results = mysqli_num_rows($result);  
	
    if($num_results > 0)
    {
		echo "Email already taken.";
		return;
    }

    $encrypted_pass = password_hash($password, PASSWORD_BCRYPT);
	$insert_data = "INSERT INTO user (First,Last,Email,Password,`Rank`) values('$first_name', '$last_name', '$email', '$encrypted_pass', '$is_teacher')";
    $data_check = mysqli_query($con, $insert_data);

    if($data_check){
        echo "Success";
    }else{
        echo "Failed inserting data into database!";
    }
?>
