USE `teliconlatestdb`;
DROP procedure IF EXISTS `PaymentDetailStatement`;

USE `teliconlatestdb`;
DROP procedure IF EXISTS `teliconlatestdb`.`PaymentDetailStatement`;
;

DELIMITER $$
USE `teliconlatestdb`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `PaymentDetailStatement`(in fromdate date,in todate date)
BEGIN
set @row_num = 0;

SELECT 
	@row_num:=@row_num + 1 AS RN,
	Workid,
	Wo_ref,
	Wo_title,
	Dispatchdt,
	Status,
	DateSubmitted,
	PaidDate,
	dateVerified,
	ConID,
	Name
FROM
	(SELECT DISTINCT
		wo.Workid,
			wo.Wo_ref,
			Wo_title,
			wo.Dispatchdt,
			wo.Status,
			wo.DateSubmitted,
			wo.PaidDate,
			wo.dateVerified,
			cd.ConID,
			CD.LastName AS Name
	FROM
		trn23100 AS wo
	INNER JOIN adm03400 CID ON wo.Workid = cid.WorkOrderId
	INNER JOIN adm03300 CD ON CID.ContractorID = CD.ConID
	WHERE
		(wo.PaidDate >= fromdate
			AND wo.PaidDate <= todate)
			AND CID.CrewLead = 1) A;
  
END$$

DELIMITER ;
;

