<?php
include 'connection.php';
       
    $module_id =  $_GET['mid'];
    $user_id =  $_GET['uid'];
    $password = $_GET['pass'];
    {//security protocol
        $security_id = $user_id;
        $security_pass = $password;
        include 'Security.php';

    }


    $query =     "SELECT *
                  FROM assignment
                  WHERE Module_ID = '$module_id'
                  ";
    $result = mysqli_query($con, $query);
    $num_results = mysqli_num_rows($result);  
	
    if($num_results === 0)
    {
        echo "Module does not contain an assignment.";
		return;
    }
    
    $row = mysqli_fetch_assoc($result);
    $hw_id = $row['ID'];

    {//check if test turnedin
        $query2 =    "SELECT *
                      FROM hw_turned_in
                      WHERE Assignment_ID = '$hw_id' AND User_ID = '$user_id'
                      ";
        $result2 = mysqli_query($con, $query2);
        $num_results2 = mysqli_num_rows($result2);  
        if($num_results2 > 0)
        {
            echo "You have already turned this assignment in.";
		    return;
        }
    }
    echo  '/' . $row['ID'] . '|' . $row['Due'] . '|' . $row['Question'];
    

?>
