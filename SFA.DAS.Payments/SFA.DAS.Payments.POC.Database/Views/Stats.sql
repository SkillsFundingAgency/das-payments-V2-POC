CREATE VIEW [dbo].[Stats] AS 
select top 100 percent
	BatchId,
	max(Progress) Progress,
	format(sum(OutsideProxy + OutsideCall)/10000, 'N0') [M3 - CallerTotal ms],
	format(avg(OutsideProxy + OutsideCall)/10000.0, 'N4') [CallerAverage ms],
	format(sum(InsideRead + InsideCalc + InsideWrite)/10000, 'N0') [CalleeTotal ms],
	format(avg(InsideRead + InsideCalc + InsideWrite)/10000.0, 'N4') [M2 - CalleeAverage ms],
	format(avg(InsideCalc)/10000.0, 'N4') [M1 - CalcAverage ms],
	format(sum(WriteMetrics)/10000, 'N0') [MetricWriteTotal ms],
	format(avg(WriteMetrics)/10000.0, 'N4') [MetricWriteAverage ms],
	count(distinct Actor) Actors,
	count(*) [Earnings Processed]
from
	Metric with (nolock)
group by
	BatchId
having
	max(Progress) > .95
--	--and 
--	count(distinct Actor) = 5
order by BatchId desc