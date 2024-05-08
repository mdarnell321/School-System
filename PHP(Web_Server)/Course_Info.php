<?php
include 'connection.php';
   


    $class_id = $_GET['cid'];
    $user_id = $_GET['uid'];
    $password = $_GET['pass'];

    {//security protocol
        $security_id = $user_id;
        $security_pass = $password;
        include 'Security.php';

    }
    


    {//check if semester started
        $query = "SELECT u.Rank FROM user AS u WHERE ID = '$user_id'";
        $result = mysqli_query($con, $query);
        $num_results = mysqli_num_rows($result);  
	
        if($num_results === 0)
        { 
          
            echo "Error.";
		    return;
        }
        else
        {
             $row = mysqli_fetch_assoc($result);
             if( $row['Rank'] == 0)
             {
                date_default_timezone_set('America/Chicago');
                $curtime = date('m-d');
                if($curtime <= '01-12' && $curtime >= '12-12' || $curtime <= '08-12' && $curtime >= '05-12')
                {
                    echo "Semester has not started yet. If you are in spring wait til January 12, for winter - December 12.";
                    return;
                }
             }
        } 
    }
   

	$query = "SELECT m.ID, m.Name, m.Description 
              FROM class AS c, modules AS m 
              WHERE c.ID = '$class_id' AND m.Class_id = '$class_id' AND c.Professor_id = m.Creator_id
              ";
    $result = mysqli_query($con, $query);
    $num_results = mysqli_num_rows($result);  
	
    if($num_results === 0)
    {
        echo "/";
		return;
    }
   
  
    $returnecho = "/";
    while($row = mysqli_fetch_assoc($result)) {
        $has_test = 0;
        $has_hw = 0;

        $test_id = 0;
        $hw_id = 0;

        $test_grade= "";
        $hw_grade= "";

        $hw_due = "";
        $test_due = "";
        {//see if it has a test
            $theid = $row['ID'];
            $query_2 =   "SELECT t.Module_ID, t.ID, t.Due
                          FROM test AS t
                          WHERE Module_ID = '$theid'
                  ";
            $result2 = mysqli_query($con, $query_2);
            $num_results2 = mysqli_num_rows($result2);
            if($num_results2 > 0)
            {
                $row2 = mysqli_fetch_assoc($result2);
                 $has_test = 1;
                 $test_id = $row2['ID'];
                 $test_due = $row2['Due'];
            }
               
            else
                $has_test = 0;


        }
         {//see if it has hw
            $theid = $row['ID'];
            $query_2 =   "SELECT a.Module_ID, a.ID, a.Due
                          FROM assignment AS a
                          WHERE Module_ID = '$theid'
                  ";
            $result2 = mysqli_query($con, $query_2);
            $num_results2 = mysqli_num_rows($result2);
            if($num_results2 > 0)
            {
                $row2 = mysqli_fetch_assoc($result2);
                 $has_hw = 1;
                 $hw_id = $row2['ID'];
                 $hw_due = $row2['Due'];
            }
               
            else
                $has_hw = 0;
        }
        if($has_test === 1)
        {//see if it has a grade
            $query_2 =   "SELECT t.Grade
                          FROM test_turned_in AS t
                          WHERE Test_ID = '$test_id' AND User_ID = '$user_id'
                  ";
            $result2 = mysqli_query($con, $query_2);
            $num_results2 = mysqli_num_rows($result2);
            if($num_results2 > 0)
            {
                  $row2 = mysqli_fetch_assoc($result2);
                  $test_grade =  $row2['Grade'] . '%';
            }
        }
        if($has_hw === 1)
        {//see if it has a grade
            $query_2 =   "SELECT h.Grade
                          FROM hw_turned_in AS h
                          WHERE Assignment_ID = '$hw_id' AND User_ID = '$user_id AND Grade IS NOT NULL'
                  ";
            $result2 = mysqli_query($con, $query_2);
            $num_results2 = mysqli_num_rows($result2);
            if($num_results2 > 0)
            {
                $row2 = mysqli_fetch_assoc($result2);
                if($row2['Grade'] != "")
                    $hw_grade =  $row2['Grade'] . '%';
            }
        }
        $returnecho .= $row['ID'] . '|' . $row['Name'] . '|' . $row['Description'] . '|' . $has_test . '|' . $test_grade. '|' . $has_hw . '|' . $hw_grade . '|' . $test_due . '|' . $hw_due . '\\';
    }
    echo $returnecho;

?>
