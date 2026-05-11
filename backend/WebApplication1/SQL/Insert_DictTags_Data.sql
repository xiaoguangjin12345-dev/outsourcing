USE [OutsourcingDB];
GO

-- 添加技能标签基础数据
INSERT INTO [DictTags] ([TagName]) VALUES 
-- 后端开发
('Java'), ('Spring Boot'), ('Spring Cloud'), ('微服务'), ('MySQL'), ('Redis'), 
('Docker'), ('Kubernetes'), ('Linux'), ('Node.js'), ('Go'), ('Python'), 
('C++'), ('.NET Core'), ('RabbitMQ'), ('Kafka'), ('Nginx'),

-- 前端开发
('Vue.js'), ('React'), ('TypeScript'), ('JavaScript'), ('HTML5'), ('CSS3'), 
('Next.js'), ('小程序开发'), ('移动端开发'),

-- 数据库
('SQL Server'), ('PostgreSQL'), ('MongoDB'), ('Oracle'), ('Elasticsearch'), 
('ClickHouse'), ('数据仓库'), ('ETL'),

-- 算法
('算法优化'), ('机器学习'), ('深度学习'), ('PyTorch'), ('TensorFlow'), 
('自然语言处理'), ('计算机视觉'), ('数据分析'), ('Pandas'), ('NumPy'),

-- 云计算与运维
('AWS'), ('阿里云'), ('DevOps'), ('CI/CD'),

-- 设计相关
('UI设计'), ('UX设计'),

-- 项目管理与文档
('文档编写'), ('项目管理'),

-- 新兴方向
('物联网'), ('信息安全'),

-- 软技能与规范
('自动化测试'), ('系统架构设计')

GO