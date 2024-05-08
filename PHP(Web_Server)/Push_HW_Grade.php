<?php
include 'connection.php';
       
	$teacher_id = $_GET['tid'];
    $graded_id= $_GET['gid'];
    $module_id= $_GET['mid'];
    $grade =  $_GET['grade'];
    $password = $_GET['pass'];
    {//security protocol
        $security_id = $teacher_id;
        $security_pass = $password;
        include 'Security.php';

    }

    {
	    $query = "SELECT c.Professor_id
                  FROM class AS c, modules AS m
                  WHERE c.ID = m.Class_id AND m.ID = '$module_id'";
        $result = mysqli_query($con, $query);
        $num_results = mysqli_num_rows($result);  
	
        if($num_results === 0)
        {
		    echo "Undefined class.";
		    return;
        }
        else {
	        $row = mysqli_fetch_assoc($result);
            if($row['Professor_id'] !== $teacher_id)
            {
                echo "You do not teach this class. Operation failed.";
		        return;
            }
        }

    }
    $query = 
    
    "UPDATE hw_turned_in as h
     INNER JOIN assignment as a
        ON h.Assignment_ID = a.ID AND a.Module_ID = '$module_id'
     SET Grade = '$grade', Graded_By = '$teacher_id'
     WHERE h.User_ID = '$graded_id'
    ";

    $result = mysqli_query($con, $query);
    if($result)
    {
		echo "/Success.";
		return;
    }
    
?>
