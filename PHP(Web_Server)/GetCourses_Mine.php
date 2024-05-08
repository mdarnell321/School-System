<?php
include 'connection.php';

    $user_id = $_GET['uid'];
	$password = $_GET['pass'];
    {//security protocol
        $security_id = $user_id;
        $security_pass = $password;
        include 'Security.php';

    }

	$query = "
	SELECT c.ID, c.Name, c.Professor_id, cei.Grade, c.Hours
	FROM class_enrolled_in AS cei, class AS c
	WHERE c.ID = cei.Class_ID AND cei.User_ID = '$user_id';
	";

    $result = mysqli_query($con, $query);
    $count = mysqli_num_rows($result);  
	$return_val = "/";
    if($count > 0)
    {
		while($row = mysqli_fetch_assoc($result)) {
			$return_val .=  $row['ID'] . '|' . $row['Name'] . '|' . $row['Professor_id'] . '|' . $row['Grade'] .'|' . $row['Hours'] . '\\';
		}
    }
	echo $return_val;
?>
