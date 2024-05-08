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
    
    {
	    $query = "SELECT DISTINCT c.Professor_id FROM class AS c, modules AS m WHERE m.ID = '$module_id'  AND m.Class_id = c.ID";
        $result = mysqli_query($con, $query);
        $num_results = mysqli_num_rows($result);  
	
        if($num_results === 0)
        {
		    echo "Undefined class.";
		    return;
        }
        else {
	        $row = mysqli_fetch_assoc($result);
            if($row['Professor_id'] !== $user_id)
            {
                echo "You do not teach this class. Operation failed.";
		        return;
            }
        }

    }

    $query =     "DELETE FROM modules
                  WHERE ID = '$module_id'
                  ";
    mysqli_query($con, $query);
    echo "/";

?>
