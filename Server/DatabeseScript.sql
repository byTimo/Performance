--Создаём базу данных, если её нет
use master
if not exists(select * from sys.sysdatabases where name = 'PerformanceDb')
  create database PerformanceDb
go