USE `teliconlatestdb`;
DROP procedure IF EXISTS `WorkOrderSummary`;

USE `teliconlatestdb`;
DROP procedure IF EXISTS `teliconlatestdb`.`WorkOrderSummary`;
;

DELIMITER $$
USE `teliconlatestdb`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `WorkOrderSummary`(in type char,in year int)
BEGIN

set @row_num = 0;

if(type = 'f') then

SELECT @row_num := convert(@row_num + 1,decimal) AS Period, 
	   round(SUM(con),2) Payments,
       round(SUM(tel),2) Revenue,
       '' as PeriodName
		FROM 
        (SELECT date_add('1900-01-01',INTERVAL ((ROUND(DATEDIFF(b.PaidDate,'1900-01-01')/7, 0)/2)*2) - 1 week)  as date,
					(a.OActQty * c.Amount * b.Wo_split2 / 100) as con, 
					(a.OActQty * c.Amount * b.Wo_split / 100) as tel
				 FROM trn23110 a inner join trn23100 b on a.WorkOID = b.Workid
				 JOIN (SELECT x.RateHistoryID, x.RateID, x.RateDescr,x.RateUnit, x.Amount, x.StartDate, x.EndDate 
					FROM (SELECT a.RateHistoryID, a.RateID, b.RateDescr, a.Amount, a.StartDate, a.EndDate, b.RateUnit 
						  FROM adm01250 a
						  JOIN adm01100 b ON a.RateID = b.RateID) AS x 
					) AS c ON c.RateHistoryID = (SELECT RateHistoryID
                                               FROM adm01250 AS c
                                               WHERE (a.ActivityID = RateID) AND (a.ActDate >= StartDate) AND (a.ActDate <= EndDate) limit 1) 
				 WHERE YEAR(b.PaidDate) = year
                 ) as tbl
		GROUP BY date
		ORDER BY Period;
        
elseif (type = 'y') then

	   SELECT convert(year(date),decimal) AS Period, 
	   round(SUM(con),2) Payments,
       round(SUM(tel),2) Revenue,
       '' as PeriodName
		FROM 
        (SELECT b.PaidDate as date,
					(a.OActQty * c.Amount * b.Wo_split2 / 100) as con, 
					(a.OActQty * c.Amount * b.Wo_split / 100) as tel
				 FROM trn23110 a inner join trn23100 b on a.WorkOID = b.Workid
				 JOIN (SELECT x.RateHistoryID, x.RateID, x.RateDescr,x.RateUnit, x.Amount, x.StartDate, x.EndDate 
					FROM (SELECT a.RateHistoryID, a.RateID, b.RateDescr, a.Amount, a.StartDate, a.EndDate, b.RateUnit 
						  FROM adm01250 a
						  JOIN adm01100 b ON a.RateID = b.RateID) AS x 
					) AS c ON c.RateHistoryID = (SELECT RateHistoryID
                                               FROM adm01250 AS c
                                               WHERE (a.ActivityID = RateID) AND (a.ActDate >= StartDate) AND (a.ActDate <= EndDate) limit 1)
                 ) as tbl
		GROUP BY year(date)
		ORDER BY Period;
ELSE
       SELECT cast((case when type = 'w' then week(date) + 1
					 when type = 'm' then month(date)
                     when type = 'q' then quarter(date) 
                     else year(date) end) as decimal) AS Period, 
	   round(SUM(con),2) Payments,
       round(SUM(tel),2) Revenue,
       '' as PeriodName
		FROM 
        (SELECT b.PaidDate as date,
					(a.OActQty * c.Amount * b.Wo_split2 / 100) as con, 
					(a.OActQty * c.Amount * b.Wo_split / 100) as tel
				 FROM trn23110 a inner join trn23100 b on a.WorkOID = b.Workid
				 JOIN (SELECT x.RateHistoryID, x.RateID, x.RateDescr,x.RateUnit, x.Amount, x.StartDate, x.EndDate 
					FROM (SELECT a.RateHistoryID, a.RateID, b.RateDescr, a.Amount, a.StartDate, a.EndDate, b.RateUnit 
						  FROM adm01250 a
						  JOIN adm01100 b ON a.RateID = b.RateID) AS x 
					) AS c ON c.RateHistoryID = (SELECT RateHistoryID
                                               FROM adm01250 AS c
                                               WHERE (a.ActivityID = RateID) AND (a.ActDate >= StartDate) AND (a.ActDate <= EndDate) limit 1)
				 WHERE YEAR(b.PaidDate) = year and (b.Status = 'r' OR b.Status = 'i' OR b.Status = 'v')
                 ) as tbl
		GROUP BY cast((case when type = 'w' then week(date) + 1
					 when type = 'm' then month(date)
                     when type = 'q' then quarter(date) 
                     else year(date) end) as decimal)
		ORDER BY Period;
        
end if;
END$$

DELIMITER ;
;