<?php
include 'connection.php';
       
    $teacher = $_GET['teach'];
	$module_name = $_GET['mn'];
    $module_description = $_GET['md'];
    $class_id = $_GET['cid'];
    $test_data = $_GET['testdata'];
    $hw_data = $_GET['hwdata'];
    $password = $_GET['pass'];
    {//security protocol
        $security_id = $teacher;
        $security_pass = $password;
        include 'Security.php';

    }


    {
	    $query = "SELECT c.Professor_id FROM class AS c WHERE ID = '$class_id'";
        $result = mysqli_query($con, $query);
        $num_results = mysqli_num_rows($result);  
	
        if($num_results === 0)
        {
		    echo "Undefined class.";
		    return;
        }
        else {
	        $row = mysqli_fetch_assoc($result);
            if($row['Professor_id'] !== $teacher)
            {
                echo "You do not teach this class. Operation failed.";
		        return;
            }
        }

    }
    $fetched_module_id = -1;
    {
        {
            $insert_data = "INSERT INTO modules (Class_id,Creator_id,Name,Description) values('$class_id', '$teacher', '$module_name', '$module_description')";
            $data_check = mysqli_query($con, $insert_data);
            if($data_check){
                 $fetched_module_id = mysqli_insert_id($con);
            }
            else {
	            echo "Error in creating module.";
                return;
            }
        }
      
    }
    if($test_data !== "")
    {
        $fetched_test_id = -1;
        {
            $arr = explode('|', $test_data);
            $test_due = $arr[2];
            $test_time = $arr[1];

            {
                $insert_data = "INSERT INTO test (Module_ID,Due,Time_Limit) values('$fetched_module_id', '$test_due', '$test_time')";
                $data_check = mysqli_query($con, $insert_data);
                if(!$data_check){
                    $query = "DELETE FROM modules WHERE ID='$fetched_module_id'";
                    mysqli_query($con, $query);

	                echo "Error in creating test.";
                    return;
                }
                else {
                    $fetched_test_id = mysqli_insert_id($con);
                }

            }

            $test_qs = explode('~', $arr[0]);
            for ($i = 0; $i < (count($test_qs) - 1); $i++) { // insert test questions after creation of test
                $test_qs_details = explode('`', $test_qs[$i]);
                if(count($test_qs_details) < 3)
                    continue;
                $answer = $test_qs_details[2];
                $options = $test_qs_details[1];
                $options_arr = preg_split('/\r\n|\r|\n/', $options);
                for ($ii = 0; $ii < (count($options_arr) ); $ii++) {
                    $option_title = $options_arr[$ii];
                    $insert_data = "INSERT INTO test_question_options values('$fetched_test_id', '$i', '$ii', '$option_title')";
                    $data_check = mysqli_query($con, $insert_data);
                    if(!$data_check){
                        $query = "DELETE FROM modules WHERE ID='$fetched_module_id'";
                        mysqli_query($con, $query);
	                    echo "Error in creating test (D).";
                        return;
                    }
                }
                $question = $test_qs_details[0];
                $insert_data = "INSERT INTO test_question (Test_ID,ID,Question, Answer) values('$fetched_test_id', '$i', '$question', '$answer')";
                $data_check = mysqli_query($con, $insert_data);
                if(!$data_check){
                    $query = "DELETE FROM modules WHERE ID='$fetched_module_id'";
                    mysqli_query($con, $query);
	                echo "Error in creating test (C).";
                    return;
                }
            }
        }
    }
    if($hw_data !== "")
    {
        {
            $arr = explode('|', $hw_data);
            $hw_due = $arr[1];
            $q = $arr[0];
            {
                $insert_data = "INSERT INTO assignment (Module_ID,Due, Question) values('$fetched_module_id', '$hw_due', '$q')";
                $data_check = mysqli_query($con, $insert_data);
                if(!$data_check){
                    $query = "DELETE FROM modules WHERE ID='$fetched_module_id'";
                    mysqli_query($con, $query);

	                echo "Error in creating HW.";
                    return;
                }
            }
        }
    }
    echo "/success";
?>
