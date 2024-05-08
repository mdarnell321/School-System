<?php
include 'connection.php';
       
	$user_id = $_GET['uid'];
    $class_branch = $_GET['cbr'];
    $class_number = $_GET['cn'];
    $course_id = -1;
    $password = $_GET['pass'];
    {//security protocol
        $security_id = $user_id;
        $security_pass = $password;
        include 'Security.php';

    }

    $is_teacher = (mysqli_num_rows(mysqli_query($con, "SELECT ID FROM user WHERE ID = '$user_id' AND `Rank` = '1'")) !== 0);
    date_default_timezone_set('America/Chicago');
    if($is_teacher === true)//convert old class dates to new
    {
          $query = "SELECT DISTINCT t.Due, t.ID
                      FROM test AS t, modules AS m
                      WHERE m.Creator_id = '$user_id' AND t.Module_ID = m.ID
            ";
            $result = mysqli_query($con, $query);
            $num_results = mysqli_num_rows($result);  
	
            if($num_results !== 0)
            {
                while($row = mysqli_fetch_assoc($result)) {
                    $_due =  strtotime($row['Due']);
                    $_id = $row['ID'];
                    $due_date = date('Y-') . date('m-d H:i:s', $_due);
                    if(date('m', $_due) < 6)// its spring semester
                    {
                        if(date('m') > 6) // we need to adjust
                        {
                            $offset = date('m',$_due) - 1;
                            $due_date = date('Y-') . (8+$offset) . '-' . date('d H:i:s', $_due);
                        }
                    }
                    else // its fall
                    {
                        if(date('m') < 6) // we need to adjust
                        {
                            $offset = date('m',$_due) - 8;
                            $due_date = date('Y-') . (1+$offset)  . '-' . date('d H:i:s', $_due);
                        }
                    }
                    
                    $query2 = 
    
                    "UPDATE test
                     SET Due = '$due_date'
                     WHERE ID = '$_id'
                    ";

                    $result2 = mysqli_query($con, $query2);
                    if(!$result2)
                    {
                         echo "Error (C).";
                         return;
                    }
                       
                }
            }
    }

    {
	    $query = "SELECT c.ID, c.Professor_id FROM class AS c WHERE Course_Branch = '$class_branch' AND Course_Number = '$class_number'" . ($is_teacher === true ? " AND Professor_id IS NULL":"");
        $result = mysqli_query($con, $query);
        $num_results = mysqli_num_rows($result);  
	
        if($num_results === 0)
        {
		    echo "Error (A).";
		    return;
        }
        $row = mysqli_fetch_assoc($result);
	    $course_id = $row['ID'];

    }
    {
	    $query = "SELECT * FROM class_enrolled_in WHERE Class_ID = '$course_id' AND User_ID = '$user_id'";
        $result = mysqli_query($con, $query);
        $num_results = mysqli_num_rows($result);  
	
        if($num_results > 0)
        {
		    echo "You are already enrolled in this course.";
		    return;
        }
        $insert_data = "INSERT INTO class_enrolled_in (Class_ID,User_ID) values('$course_id', '$user_id')";
        $data_check = mysqli_query($con, $insert_data);
        if($data_check){
           

        }
        else {
	        echo "Error in enrolling.";
            return;
        }
      
    }
   
        $curtime = date('Y-m-d H:i:s');
        {//here we are gonna populate zeros if joining class late (tests)
            $query = "SELECT DISTINCT t.ID 
                      FROM test AS t, modules as m, class AS c
                      WHERE t.Module_ID = m.ID AND m.Class_id = '$course_id' AND c.Professor_id = m.Creator_id AND t.Due <= '$curtime'";
            $result = mysqli_query($con, $query);
            $num_results = mysqli_num_rows($result);  
	
            if($num_results !== 0)
            {
                while($row = mysqli_fetch_assoc($result)) {
                    $_testid = $row['ID'];
                    $insert_data = "INSERT INTO test_turned_in (Test_ID,User_ID, Stamp, Grade) values('$_testid', '$user_id', '$curtime', '0')";
                    $data_check = mysqli_query($con, $insert_data);
                    if(!$data_check){// redact our previous enrollment insertion
                        $delquery = "DELETE FROM class_enrolled_in WHERE Class_ID='$course_id' AND User_ID = '$user_id'";
                        mysqli_query($con, $delquery);
                        { // delete all test turn ins
                            $delquery2 = "DELETE FROM test_turned_in WHERE Test_ID='$_testid' AND User_ID = '$user_id'";
                            mysqli_query($con, $delquery2);
                        }
	                    echo "Error in enrolling (2).";
                        return;
                    }
                }
            }
        }
        {//here we are gonna populate zeros if joining class late (hw)
            $query = "SELECT DISTINCT a.ID 
                      FROM assignment AS a, modules as m, class AS c
                      WHERE a.Module_ID = m.ID AND m.Class_id = '$course_id' AND c.Professor_id = m.Creator_id AND a.Due <= '$curtime'";
            $result = mysqli_query($con, $query);
            $num_results = mysqli_num_rows($result);  
	
            if($num_results !== 0)
            {
           
                while($row = mysqli_fetch_assoc($result)) {
                    $_hwid = $row['ID'];
                    $insert_data = "INSERT INTO hw_turned_in (Assignment_ID,User_ID, Stamp, Grade) values('$_hwid', '$user_id', '$curtime', '0')";
                    $data_check = mysqli_query($con, $insert_data);
                    if(!$data_check){// redact our previous enrollment insertion
                        $delquery = "DELETE FROM class_enrolled_in WHERE Class_ID='$course_id' AND User_ID = '$user_id'";
                        mysqli_query($con, $delquery);
                        { // delete all test turn ins
                            $delquery2 = "DELETE FROM hw_turned_in WHERE Assignment_ID='$_hwid' AND User_ID = '$user_id'";
                            mysqli_query($con, $delquery2);
                        }
	                    echo "Error in enrolling (2).";
                        return;
                    }
                }
            }
        }
        if($is_teacher)
        {
             $query = 
    
            "UPDATE class
             SET Professor_id = '$user_id'
             WHERE Course_Branch = '$class_branch' AND Course_Number = '$class_number' AND Professor_id IS NULL
            ";

            $result = mysqli_query($con, $query);
            if(!$result)
                echo "Error (B).";
        }
        echo "/Success";
?>
