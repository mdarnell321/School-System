<?php

       
	$query = "
	SELECT Password 
	FROM user 
	WHERE ID = '". $security_id ."'
			";

    $result = mysqli_query($con, $query);
    $count = mysqli_num_rows($result);  
	
    if($count === 1)
    {
		$row = mysqli_fetch_assoc($result);
		$fetch_pass = $row['Password'];
        if(!password_verify($security_pass, $fetch_pass)){
			echo "Access denied.";
			exit();
		}
    }
	else {
		echo "Access denied.";
		exit();
	}

    
?>
