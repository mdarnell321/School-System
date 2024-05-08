<?php
include 'connection.php';
       
	$user_id = $_GET['uid'];
    $answer= $_GET['ans'];
    $module_id = $_GET['mid'];
    $password = $_GET['pass'];
    {//security protocol
        $security_id = $user_id;
        $security_pass = $password;
        include 'Security.php';

    }
    date_default_timezone_set('America/Chicago');
    $curtime = date('Y-m-d H:i:s');

    $hw_id = 0;

    {
        $query =    "SELECT a.ID
                      FROM assignment AS a
                      WHERE Module_ID = '$module_id'
                      ";
        $result = mysqli_query($con, $query);
        $num_results = mysqli_num_rows($result);  
        if($num_results === 0)
        {
            echo "Something went wrong.";
		    return;
        }
        $row = mysqli_fetch_assoc($result);
        $hw_id = $row['ID'];
    }

        $insert_data = "INSERT INTO hw_turned_in VALUES('$user_id', '$hw_id', '$curtime', '$answer', null, null)";
        $data_check = mysqli_query($con, $insert_data);
        if($data_check){
            echo "/Success";
        }
        else {
	        echo "Error in turning in assignment.";
        }

    
?>
